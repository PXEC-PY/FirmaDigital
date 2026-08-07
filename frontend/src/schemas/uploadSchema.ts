import { z } from 'zod'

export const MAX_FILE_SIZE_BYTES = 20 * 1024 * 1024

export const uploadSchema = z.object({
  file: z
    .instanceof(File, { message: 'Debe seleccionar un archivo.' })
    .refine((file) => file.type === 'application/pdf', 'Solo se admiten archivos PDF.')
    .refine(
      (file) => file.size <= MAX_FILE_SIZE_BYTES,
      `El archivo supera el tamaño máximo permitido de ${MAX_FILE_SIZE_BYTES / (1024 * 1024)} MB.`,
    ),
})

export type UploadFormValues = z.infer<typeof uploadSchema>
