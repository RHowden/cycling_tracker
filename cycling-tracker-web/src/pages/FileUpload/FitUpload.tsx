import { useRef, useState, type DragEvent, type KeyboardEvent } from "react";
import "./FitUpload.css";
import { useFitUpload } from "../../hooks/useFitUpload";

export function FitUpload() {
  const inputRef = useRef<HTMLInputElement>(null);
  const [dragging, setDragging] = useState(false);
  const { items, upload } = useFitUpload();

  function onDrop(e: DragEvent<HTMLDivElement>) {
    e.preventDefault();
    setDragging(false);
    upload(e.dataTransfer.files);
  }

  function onKeyDown(e: KeyboardEvent<HTMLDivElement>) {
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      inputRef.current?.click();
    }
  }

  return (
    <div className="fit-upload">
      <div
        className={`fit-upload__drop ${dragging ? "is-dragging" : ""}`}
        onClick={() => inputRef.current?.click()}
        onKeyDown={onKeyDown}
        onDragOver={(e) => {
          e.preventDefault();
          setDragging(true);
        }}
        onDragLeave={() => setDragging(false)}
        onDrop={onDrop}
        role="button"
        tabIndex={0}
      >
        Drop FIT files here, or click to choose
        <input
          ref={inputRef}
          type="file"
          accept=".fit"
          multiple
          hidden
          onChange={(e) => {
            upload(e.target.files);
            e.target.value = ""; // allow re-selecting the same file
          }}
        />
      </div>

      <ul className="fit-upload__list">
        {items.map((i) => (
          <li key={i.id} className={`fit-upload__item is-${i.status}`}>
            <span className="fit-upload__name">{i.name}</span>
            {i.status === "uploading" &&  <span>Uploading</span>}
            {i.status === "done" && <span>Uploaded</span>}
            {i.status === "error" && <span role="alert">{i.error}</span>}
          </li>
        ))}
      </ul>
    </div>
  );
}
