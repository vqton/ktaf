from flask import Flask
from .extensions import db, migrate, jwt, celery


def create_app(config_module=None):
    """
    Application Factory Pattern
    
    Returns:
        Flask: Initialized Flask application
    """
    app = Flask(__name__)
    
    # Configure app
    app.config.from_object(config_module or 'app.config.Config')
    
    # Initialize extensions
    db.init_app(app)
    migrate.init_app(app, db)
    jwt.init_app(app)
    celery.conf.update(app.config.get('CELERY_CONFIG', {}))
    celery.app = app
    
    # Register blueprints
    from .modules.auth.routes import auth_blueprint
    from .modules.nhat_ky.routes import nhat_ky_blueprint
    app.register_blueprint(auth_blueprint, url_prefix='/api/auth')
    app.register_blueprint(nhat_ky_blueprint, url_prefix='/api/nhat_ky')
    
    return app

# Create application instance
app = create_app()
"""
# Note: The triple quotes above should be the FINAL line of the file
# Make sure there are no extra quotes or characters after this line
"""
