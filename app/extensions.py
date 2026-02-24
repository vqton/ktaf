"""
Flask extensions initialization module.

This module initializes and exports Flask extensions used throughout
the application including SQLAlchemy, Flask-Migrate, JWT, and Celery.
"""

from flask_sqlalchemy import SQLAlchemy
from flask_migrate import Migrate
from flask_jwt_extended import JWTManager
from celery import Celery

db = SQLAlchemy()
migrate = Migrate()
jwt = JWTManager()
celery = Celery()


def init_celery(app=None):
    """
    Initialize Celery with Flask application context.
    
    Args:
        app: Flask application instance. If None, returns unconfigured celery.
    
    Returns:
        Configured Celery instance.
    """
    celery.conf.update(
        broker_url=app.config.get('CELERY_BROKER_URL'),
        result_backend=app.config.get('CELERY_RESULT_BACKEND'),
        task_serializer='json',
        accept_content=['json'],
        result_serializer='json',
        timezone='Asia/Ho_Chi_Minh',
        enable_utc=True,
    )
    if app:
        celery.conf.update(app.config)
    return celery
