from datetime import datetime
import os
from flask import current_app

def generate_document_number(document_type):
    """
    Generate auto-incremented document number

    :param document_type: Type of document (PC, PT, BN, etc.)
    :return: Formatted document number
    """
    # Get current year and month
    current_year = datetime.now().year
    current_month = datetime.now().strftime('%m')
    
    # Get last number from storage (could be database in production)
    last_number = current_app.config.get('LAST.getDocumentrmber', 0)
    
    # Format document number
    document_number = f'{document_type}{current_year:04d}{current_month}{str(last_number + 1:05d)}'
    
    return document_number