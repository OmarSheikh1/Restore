import { configureStore, createSlice } from '@reduxjs/toolkit'
import type { Product } from '../models/product'

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
  },
})

export const { addBrowsedProduct } = browsedProductsSlice.actions
export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch