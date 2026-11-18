import { configureStore, createSlice } from '@reduxjs/toolkit'
import { setupListeners } from '@reduxjs/toolkit/query'
import type { Product } from '../models/product'
import { productsApi } from '../features/productsRtk/api'

interface PhysicalStore {
  name: string
  address: string
  latitude: number
  longitude: number
}

const physicalStoreSlice = createSlice({
  name: 'physicalStore',
  initialState: {
    name: 'ReStore Test Store',
    address: '123 Example Street, Seattle, WA 98101',
    latitude: 47.6062,
    longitude: -122.3321,
  } satisfies PhysicalStore,
  reducers: {},
})

const browsedProductsSlice = createSlice({
  name: 'browsedProducts',
  initialState: [] as Product[],
  reducers: {
    addBrowsedProduct(state, action: { payload: Product }) {
      const existingIndex = state.findIndex(product => product.id === action.payload.id)
      if (existingIndex !== -1) state.splice(existingIndex, 1)
      state.unshift(action.payload)
    },
  },
})

export const store = configureStore({
  reducer: {
    physicalStore: physicalStoreSlice.reducer,
    browsedProducts: browsedProductsSlice.reducer,
    [productsApi.reducerPath]: productsApi.reducer,
  },
  middleware: getDefaultMiddleware => getDefaultMiddleware().concat(productsApi.middleware),
})

setupListeners(store.dispatch)

export const { addBrowsedProduct } = browsedProductsSlice.actions
export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch