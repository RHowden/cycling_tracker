import { useState } from "react";
import { uploadFitFile, validateFitFile } from "../api/fitAPI";
import type { SessionUploadResponse } from  "../api/schemas"; 

export type UploadStatus = "uploading" | "done" | "error";

export interface UploadItem {
  id: string;
  name: string;
  status: UploadStatus;
  error?: string;
}

export function useFitUpload(onUploaded?: (session: SessionUploadResponse | null) => void) {
  const [items, setItems] = useState<UploadItem[]>([]);

  const patch = (id: string, changes: Partial<UploadItem>) =>
    setItems((prev) => prev.map((i) => (i.id === id ? { ...i, ...changes } : i)));

  async function upload(fileList: FileList | null) {
    if (!fileList) return;

    for (const file of Array.from(fileList)) {
      const id = crypto.randomUUID();
      const error = validateFitFile(file);

      setItems((prev) => [
        ...prev,
        { id, name: file.name, status: error ? "error" : "uploading", error: error ?? undefined },
      ]);
      if (error) continue;

      try {
        const result = await uploadFitFile(file);
        patch(id, { status: "done" });
        onUploaded?.(result);
      } catch (err) {
        patch(id, { status: "error", error: err instanceof Error ? err.message : "Upload failed" });
      }
    }
  }

  return { items, upload };
}