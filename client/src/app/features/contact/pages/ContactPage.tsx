import { Link } from 'react-router-dom'
import { useSelector } from 'react-redux'
import type { RootState } from '../../../store/store'

export default function ContactPage() {
  const physicalStore = useSelector((state: RootState) => state.physicalStore)
  const mapUrl = `https://www.google.com/maps?q=${physicalStore.latitude},${physicalStore.longitude}`

  return (
    <section>
      <h1>Contact Us</h1>
      <h2>{physicalStore.name}</h2>
      <p>{physicalStore.address}</p>
      <p>
        Latitude: {physicalStore.latitude}, Longitude: {physicalStore.longitude}
      </p>
      <p>These are sample details for testing and do not identify a real store.</p>
      <p><a href={mapUrl} target="_blank" rel="noreferrer">View sample location on Google Maps</a></p>
      <p>You can also explore ReStore using these links:</p>
      <ul>
        <li><Link to="/products">Browse products</Link></li>
        <li><Link to="/about">Learn about ReStore</Link></li>
        <li><Link to="/">Go to the home page</Link></li>
      </ul>
    </section>
  )
}
