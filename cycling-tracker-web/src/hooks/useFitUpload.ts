import { useState } from "react";
import { uploadFitFile, validateFitFile } from "../api/fitAPI";

export type UploadStatus = "uploading" | "done" | "error";

export interface UploadItem {
  id: string;
  name: string;
  status: UploadStatus;
  error?: string;
}

export function useFitUpload(onUploaded?: () => void) {
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
        await uploadFitFile(file);
        patch(id, { status: "done" });
        onUploaded?.();
      } catch (err) {
        patch(id, { status: "error", error: err instanceof Error ? err.message : "Upload failed" });
      }
    }
  }

  return { items, upload };
}