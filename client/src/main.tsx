import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from 'react-redux'
import './app/layout/styles.css'
import AppRoutes from './app/routes/Routes'
import { store } from './app/store/store'

createRoot(document.getElementById('root')!).render(
  <Provider store={store}>
    <StrictMode>
      <AppRoutes />
    </StrictMode>
  </Provider>,
)
