import { z } from "zod";

export class ApiError extends Error {
  status: number;
  messages: string[];
  constructor(status: number, messages: string[]) {
    super(messages.join(" "));
    this.status = status;
    this.messages = messages;
  }
}

export class ResponseShapeError extends Error {
  constructor(public path: string, public error: z.ZodError) {
    super(`Response from ${path} did not match the expected shape:\n${z.prettifyError(error)}`);
    this.name = "ResponseShapeError";
  }
}