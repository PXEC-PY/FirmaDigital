import { useRef, useState, type DragEvent } from 'react'
import { formatBytes } from '../lib/format'

interface DropZoneProps {
  file: File | null
  onFileSelected: (file: File) => void
  onClear: () => void
  errorMessage?: string
  disabled?: boolean
}

export function DropZone({ file, onFileSelected, onClear, errorMessage, disabled }: DropZoneProps) {
  const [isDragOver, setIsDragOver] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)

  function handleDrop(event: DragEvent<HTMLDivElement>) {
    event.preventDefault()
    setIsDragOver(false)
    if (disabled) return
    const dropped = event.dataTransfer.files?.[0]
    if (dropped) onFileSelected(dropped)
  }

  return (
    <div>
      <div
        role="button"
        tabIndex={0}
        onClick={() => !disabled && inputRef.current?.click()}
        onKeyDown={(e) => e.key === 'Enter' && !disabled && inputRef.current?.click()}
        onDragOver={(e) => {
          e.preventDefault()
          if (!disabled) setIsDragOver(true)
        }}
        onDragLeave={() => setIsDragOver(false)}
        onDrop={handleDrop}
        className={`flex min-h-64 cursor-pointer flex-col items-center justify-center gap-4 rounded-2xl border-2 border-dashed p-10 text-center transition-colors ${
          disabled ? 'cursor-not-allowed opacity-60' : ''
        } ${
          isDragOver
            ? 'border-blue-500 bg-blue-50/60 dark:border-blue-400 dark:bg-blue-500/10'
            : 'border-slate-300 bg-slate-50/60 hover:border-slate-400 dark:border-slate-700 dark:bg-slate-900/40 dark:hover:border-slate-600'
        }`}
      >
        <input
          ref={inputRef}
          type="file"
          accept="application/pdf"
          className="hidden"
          disabled={disabled}
          onChange={(e) => {
            const selected = e.target.files?.[0]
            if (selected) onFileSelected(selected)
            e.target.value = ''
          }}
        />

        <div className="flex h-14 w-14 items-center justify-center rounded-full bg-blue-50 text-blue-600 dark:bg-blue-500/10 dark:text-blue-400">
          <svg viewBox="0 0 24 24" fill="none" className="h-7 w-7" stroke="currentColor" strokeWidth={1.5}>
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M12 16.5V4.5m0 0 4 4m-4-4-4 4M4.5 16.5v3a1.5 1.5 0 0 0 1.5 1.5h12a1.5 1.5 0 0 0 1.5-1.5v-3"
            />
          </svg>
        </div>

        {file ? (
          <div>
            <p className="text-sm font-medium text-slate-900 dark:text-slate-100">{file.name}</p>
            <p className="text-xs text-slate-500 dark:text-slate-400">{formatBytes(file.size)}</p>
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation()
                onClear()
              }}
              disabled={disabled}
              className="mt-3 text-xs font-medium text-slate-500 underline decoration-slate-300 underline-offset-2 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
            >
              Quitar archivo
            </button>
          </div>
        ) : (
          <div>
            <p className="text-sm font-medium text-slate-700 dark:text-slate-300">
              Arrastrá tu PDF acá o hacé clic para seleccionarlo
            </p>
            <p className="mt-1 text-xs text-slate-400 dark:text-slate-500">PDF · máximo 20 MB</p>
          </div>
        )}

        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation()
            !disabled && inputRef.current?.click()
          }}
          disabled={disabled}
          className="rounded-full border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50 disabled:opacity-60 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-200 dark:hover:bg-slate-700"
        >
          Seleccionar PDF
        </button>
      </div>

      {errorMessage && (
        <p className="mt-2 text-sm text-red-600 dark:text-red-400">{errorMessage}</p>
      )}
    </div>
  )
}
