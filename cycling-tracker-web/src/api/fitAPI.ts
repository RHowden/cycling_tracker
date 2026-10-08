import { SessionUploadResponse, SessionUploadResponseSchema } from "../api/schemas"
import { apiRequest } from "./client";
const MAX_SIZE_MB = 20;

export function validateFitFile(file: File): string | null {
    if (!file.name.toLowerCase().endsWith(".fit")) return "Only .fit files are supported";
    if (file.size > MAX_SIZE_MB * 1024 * 1024) return `File is larger than ${MAX_SIZE_MB} MB`;
    return null;
}

export async function uploadFitFile(file: File): Promise<SessionUploadResponse> {
    const form = new FormData();
    form.append("file", file);

    return apiRequest("/import/import", SessionUploadResponseSchema, { method: "POST", body: form });
}

