class Config:
    # General Settings
    SECRET_KEY = "your-secret-key-here"
    TESTING = False
    DEBUG = False
    ENV = "production"

    # Database Configuration
    SQLALCHEMY_DATABASE_URI = "mariadb://user:password@localhost/db_name"
    SQLALCHEMY_DATABASE_URI_TEST = "sqlite:///test.db"
    SQLALCHEMY_DATABASE_URI_PROD = (
        "mariadb://prod_user:prod_password@prod_db_host:3306/prod_db_name"
    )
    SQLALCHEMY_TRACK_MODIFICATIONS = False

    # JWT Settings
    JWT_SECRET_KEY = "super-secret-jwt-key"
    JWT_ACCESS_TOKEN_EXPIRES = 3600  # 1 hour
    JWT_REFRESH_TOKEN_EXPIRES = 2592000  # 30 days

    # Celery Configuration
    CELERY_CONFIG = {
        "broker_url": "redis://localhost:6379/0",
        "result_backend": "redis://localhost:6379/0",
        "task_track_started": True,
        "max_retries": 3,
    }
