import os
from app import create_app
from app.extensions import celery

app = create_app(os.environ.get('FLASK_ENV', 'development'))


@celery.task
def test_task():
    return {'status': 'completed'}


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
