import boto3
import json
import requests
import sys

def receive_and_send_response(res):
    sqs = boto3.client('sqs')

    try:
        response = sqs.send_message(
            QueueUrl="https://sqs.us-east-1.amazonaws.com/304850274251/SchedulingOutputQueue.fifo",
            MessageBody=json.dumps(res)
        )

    except Exception as e:
        raise

def handle_input_SQS_message():
    sqs = boto3.client('sqs')
    while True:
        try:
            response = sqs.receive_message(
                QueueUrl="https://sqs.us-east-1.amazonaws.com/304850274251/SchedulingInputQueue.fifo",
            )

            if 'Messages' in response:
                for message in response['Messages']:
                    try:
                        message_body = json.loads(message['Body'])
                        response = requests.post('/Schedules', json=message_body)

                        sqs.delete_message(
                            QueueUrl="https://sqs.us-east-1.amazonaws.com/304850274251/SchedulingInputQueue.fifo",
                            ReceiptHandle=message['ReceiptHandle']
                        )

                        receive_and_send_response(response)

                    except Exception as e:
                        raise
            else:
                raise
        except Exception as e:
            sys.exit()



handle_input_SQS_message()