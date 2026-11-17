import { Link } from 'react-router-dom'

export default function AboutPage() {
  return (
    <section>
      <h1>About Restore</h1>
      <p>Restore is a place to explore our collection of products.</p>
      <Link className="button-link" to="/products">Browse products</Link>
    </section>
  )
}
