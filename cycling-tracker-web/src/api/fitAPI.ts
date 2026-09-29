import type { Session } from "../types/session";
import { ApiError } from "./ApiError";
import { parseErrorMessages } from "./parseErrorMessages";
const API_URL = `${import.meta.env.VITE_API_BASE_URL}/api/sessions/import`;
const MAX_SIZE_MB = 20;

export function validateFitFile(file: File): string | null {
    if (!file.name.toLowerCase().endsWith(".fit")) return "Only .fit files are supported";
    if (file.size > MAX_SIZE_MB * 1024 * 1024) return `File is larger than ${MAX_SIZE_MB} MB`;
    return null;
}

// XMLHttpRequest rather than fetch, because fetch can't report upload progress.
// The auth cookie is sent automatically as the request is same-origin (via the Vite proxy).
export function uploadFitFileOld(
    file: File,
    onProgress: (percent: number) => void
): Promise<Session | null> {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        const form = new FormData();
        form.append("file", file); // must match the IFormFile parameter name in the API

        xhr.open("POST", API_URL);
        // Don't set Content-Type: the browser adds the multipart boundary itself.

        xhr.upload.onprogress = (e) => {
            if (e.lengthComputable) onProgress(Math.round((e.loaded / e.total) * 100));
        };
        xhr.onload = () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(xhr.responseText ? (JSON.parse(xhr.responseText) as Session) : null);
            } else {
                reject(new Error(xhr.responseText || `Upload failed (${xhr.status})`));
            }
        };
        xhr.onerror = () => reject(new Error("Network error"));
        xhr.send(form);
    });
}

const API_BASE = `${import.meta.env.VITE_API_BASE_URL}/api`;

export async function uploadFitFile(file: File): Promise<Session> {
    const form = new FormData();
    form.append("file", file);


    let res: Response;
    try {
        res = await fetch(`${API_BASE}/sessions/import`, {
            method: "POST",
            credentials: "include",
            body: form,
        });
    } catch {
        throw new ApiError(0, ["Network error — please check your connection."]);
    }

    if (!res.ok) {
        throw new ApiError(res.status, await parseErrorMessages(res));
    }

    return await (res.json()) as Session;
}