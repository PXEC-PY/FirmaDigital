import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Header } from './components/Header'
import { DropZone } from './components/DropZone'
import { LoadingStages } from './components/LoadingStages'
import { ResultView } from './components/ResultView'
import { useValidateDocument } from './hooks/useValidateDocument'
import { uploadSchema, type UploadFormValues } from './schemas/uploadSchema'
import { ApiError } from './api/client'

function App() {
  const mutation = useValidateDocument()

  const {
    watch,
    setValue,
    resetField,
    handleSubmit,
    formState: { errors },
  } = useForm<UploadFormValues>({
    resolver: zodResolver(uploadSchema),
  })

  const file = watch('file') ?? null

  function onSubmit(values: UploadFormValues) {
    mutation.mutate(values.file)
  }

  function handleReset() {
    resetField('file')
    mutation.reset()
  }

  const isLoading = mutation.isPending

  return (
    <div className="min-h-full bg-slate-50 dark:bg-slate-950">
      <Header />

      <main className="mx-auto max-w-5xl px-6 py-10">
        {mutation.isSuccess ? (
          <ResultView result={mutation.data} onValidateAnother={handleReset} />
        ) : isLoading ? (
          <LoadingStages />
        ) : (
          <form onSubmit={handleSubmit(onSubmit)} className="mx-auto max-w-xl">
            <DropZone
              file={file}
              onFileSelected={(selected) => setValue('file', selected, { shouldValidate: true })}
              onClear={() => resetField('file')}
              errorMessage={errors.file?.message}
            />

            {mutation.isError && (
              <div className="mt-4 rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700 dark:border-red-500/30 dark:bg-red-500/10 dark:text-red-400">
                {mutation.error instanceof ApiError
                  ? mutation.error.message
                  : 'Ocurrió un error inesperado al validar el documento.'}
              </div>
            )}

            <button
              type="submit"
              disabled={!file}
              className="mt-6 w-full rounded-full bg-blue-600 px-5 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-slate-300 dark:disabled:bg-slate-700"
            >
              Validar
            </button>
          </form>
        )}
      </main>
    </div>
  )
}

export default App
