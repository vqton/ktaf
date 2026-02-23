from flask_sqlalchemy import SQLAlchemy
from flask_migrate import Migrate
from flask_jwt_extended import JWTManager
from celery import Celery
import pymysql

definition_config = {
    'SQLALCHEMY_DATABASE_URI': 'mariadb+pymysql://user:password@localhost/db_name',
    'SQLALCHEMY_TRACK_MODIFICATIONS': False,
    'JWT_SECRET_KEY': 'super-secret-key'
}

prod_config = {
    'SQLALCHEMY_DATABASE_URI': 'mariadb+pymysql://prod_user:prod_password@prod_db_host:3306/prod_db_name',
    'SQLALCHEMY_TRACK_MODIFICATIONS': False,
    'JWT_SECRET_KEY': 'prod-super-secret-key'
}

test_config = {
    'SQLALCHEMY_DATABASE_URI': 'sqlite:///test.db',
    'SQLALCHEMY_TRACK_MODIFICATIONS': False,
    'JWT_SECRET_KEY': 'test-super-secret-key'
}

db = SQLAlchemy()
migrate = Migrate(db)
jwt = JWTManager()
celery = Celery()

# Explicitly import pymysql driver
import pymysql
