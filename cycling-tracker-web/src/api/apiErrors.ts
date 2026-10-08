export class ApiError extends Error {
  status: number;
  messages: string[];
  constructor(status: number, messages: string[]) {
    super(messages.join(" "));
    this.status = status;
    this.messages = messages;
  }
}

