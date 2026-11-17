import { useState } from 'react'

const productImages = import.meta.glob<string>('../../../../assets/images/products/*', {
  eager: true,
  import: 'default',
  query: '?url',
})

export function ProductImage({ src, name }: { src: string; name: string }) {
  const [failed, setFailed] = useState(false)
  if (!src || failed) return <div className="product-image-placeholder">Image unavailable</div>

  const filename = src.split(/[?#]/, 1)[0].split('/').pop()
  const localSrc = filename ? productImages[`../../../../assets/images/products/${filename}`] : undefined

  return <img className="product-image" src={localSrc ?? src} alt={name} onError={() => setFailed(true)} />
}
