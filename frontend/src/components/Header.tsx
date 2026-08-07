export function Header() {
  return (
    <header className="border-b border-slate-200/80 bg-white/80 backdrop-blur dark:border-slate-800 dark:bg-slate-950/80">
      <div className="mx-auto flex max-w-5xl items-center gap-3 px-6 py-5">
        <img src="/logo.png" alt="" className="h-10 w-10 rounded-lg object-contain" />
        <div>
          <h1 className="text-base font-semibold tracking-tight text-slate-900 dark:text-slate-100">
            Validador de Firmas Digitales del Paraguay
          </h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Verificación de integridad, cadena de confianza y revocación sobre la PKI paraguaya.
          </p>
        </div>
      </div>
    </header>
  )
}
