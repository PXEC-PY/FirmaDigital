import { useMutation } from '@tanstack/react-query'
import { validateDocument } from '../api/client'

export function useValidateDocument() {
  return useMutation({
    mutationFn: (file: File) => validateDocument(file),
  })
}
