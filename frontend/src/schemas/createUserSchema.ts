import { z } from 'zod'

export const createUserSchema = z.object({
  email: z.string().min(1, 'Ingresá un email.').email('Ingresá un email válido.'),
  nombreCompleto: z.string().min(1, 'Ingresá un nombre.'),
  password: z
    .string()
    .min(10, 'Mínimo 10 caracteres.')
    .regex(/[A-Z]/, 'Debe tener al menos una mayúscula.')
    .regex(/[a-z]/, 'Debe tener al menos una minúscula.')
    .regex(/[0-9]/, 'Debe tener al menos un número.'),
  role: z.enum(['Administrador', 'Auditor', 'Usuario']),
})

export type CreateUserFormValues = z.infer<typeof createUserSchema>
