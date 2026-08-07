export function formatDateTime(value: string | null): string {
  if (!value) return 'No disponible'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'No disponible'
  return new Intl.DateTimeFormat('es-PY', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date)
}

export function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  const units = ['KB', 'MB', 'GB']
  let value = bytes / 1024
  let unitIndex = 0
  while (value >= 1024 && unitIndex < units.length - 1) {
    value /= 1024
    unitIndex += 1
  }
  return `${value.toFixed(1)} ${units[unitIndex]}`
}

export function truncateMiddle(value: string, visibleChars = 10): string {
  if (value.length <= visibleChars * 2 + 3) return value
  return `${value.slice(0, visibleChars)}...${value.slice(-visibleChars)}`
}
