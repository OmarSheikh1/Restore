import { useState } from 'react'

export function ProductImage({ src, name }: { src: string; name: string }) {
  const [failed, setFailed] = useState(false)
  if (!src || failed) return <div className="product-image-placeholder">Image unavailable</div>
  return <img className="product-image" src={src} alt={name} onError={() => setFailed(true)} />
}
