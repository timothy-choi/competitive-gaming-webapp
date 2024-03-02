import { HubConnectionBuilder } from '@microsoft/signalr';

const SignalRService = {
  connection: null,

  startConnection: () => {
    SignalRService.connection = new HubConnectionBuilder()
      .withUrl('')
      .build();

    SignalRService.connection.start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.error('SignalR Connection Error: ', err));
  },
};

export default SignalRService;
