"""Authentication API endpoints.

Provides:
- POST /login: User login with username/password
- POST /register: User registration
- GET /me: Get current user info (JWT required)
- POST /change-password: Change password (JWT required)
"""

from flask import Blueprint, request, jsonify
from flask_jwt_extended import create_access_token, jwt_required, get_jwt_identity
from app.extensions import db
from app.modules.auth.models import User
from werkzeug.security import check_password_hash, generate_password_hash
from marshmallow import Schema, fields, validate, ValidationError

auth_bp = Blueprint('auth', __name__)


class LoginSchema(Schema):
    username = fields.Str(required=True, validate=validate.Length(min=3, max=80))
    password = fields.Str(required=True, validate=validate.Length(min=6))


class RegisterSchema(Schema):
    username = fields.Str(required=True, validate=validate.Length(min=3, max=80))
    email = fields.Email(required=True)
    password = fields.Str(required=True, validate=validate.Length(min=6))
    full_name = fields.Str(required=False, allow_none=True)


@auth_bp.route('/login', methods=['POST'])
def login():
    try:
        data = LoginSchema().load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    user = User.query.filter_by(username=data['username']).first()

    if not user or not check_password_hash(user.password_hash, data['password']):
        return jsonify({'success': False, 'message': 'Invalid username or password'}), 401

    if not user.is_active:
        return jsonify({'success': False, 'message': 'Account is inactive'}), 403

    access_token = create_access_token(identity=user.id)

    return jsonify({
        'success': True,
        'data': {
            'access_token': access_token,
            'user': {
                'id': user.id,
                'username': user.username,
                'email': user.email,
                'full_name': user.full_name,
                'is_admin': user.is_admin
            }
        },
        'message': 'Login successful'
    }), 200


@auth_bp.route('/register', methods=['POST'])
def register():
    try:
        data = RegisterSchema().load(request.get_json())
    except ValidationError as err:
        return jsonify({'success': False, 'message': 'Validation error', 'errors': err.messages}), 400

    if User.query.filter_by(username=data['username']).first():
        return jsonify({'success': False, 'message': 'Username already exists'}), 400

    if User.query.filter_by(email=data['email']).first():
        return jsonify({'success': False, 'message': 'Email already exists'}), 400

    user = User(
        username=data['username'],
        email=data['email'],
        password_hash=generate_password_hash(data['password']),
        full_name=data.get('full_name'),
        is_active=True
    )

    db.session.add(user)
    db.session.commit()

    return jsonify({
        'success': True,
        'data': {'id': user.id, 'username': user.username},
        'message': 'User registered successfully'
    }), 201


@auth_bp.route('/me', methods=['GET'])
@jwt_required()
def me():
    user_id = get_jwt_identity()
    user = User.query.get(user_id)

    if not user:
        return jsonify({'success': False, 'message': 'User not found'}), 404

    return jsonify({
        'success': True,
        'data': {
            'id': user.id,
            'username': user.username,
            'email': user.email,
            'full_name': user.full_name,
            'is_admin': user.is_admin,
            'is_active': user.is_active
        }
    }), 200


@auth_bp.route('/change-password', methods=['POST'])
@jwt_required()
def change_password():
    user_id = get_jwt_identity()
    user = User.query.get(user_id)

    if not user:
        return jsonify({'success': False, 'message': 'User not found'}), 404

    data = request.get_json()
    old_password = data.get('old_password')
    new_password = data.get('new_password')

    if not old_password or not new_password:
        return jsonify({'success': False, 'message': 'Missing password fields'}), 400

    if not check_password_hash(user.password_hash, old_password):
        return jsonify({'success': False, 'message': 'Incorrect old password'}), 400

    if len(new_password) < 6:
        return jsonify({'success': False, 'message': 'New password must be at least 6 characters'}), 400

    user.password_hash = generate_password_hash(new_password)
    db.session.commit()

    return jsonify({
        'success': True,
        'message': 'Password changed successfully'
    }), 200
