import type { Session } from "../types/session";
import { apiRequest } from "./client";
const MAX_SIZE_MB = 20;

export function validateFitFile(file: File): string | null {
    if (!file.name.toLowerCase().endsWith(".fit")) return "Only .fit files are supported";
    if (file.size > MAX_SIZE_MB * 1024 * 1024) return `File is larger than ${MAX_SIZE_MB} MB`;
    return null;
}

export async function uploadFitFile(file: File): Promise<Session> {
    const form = new FormData();
    form.append("file", file);

    return apiRequest<Session>("/sessions/import", { method: "POST", body: form });
}

