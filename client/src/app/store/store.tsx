import { configureStore, createSlice } from '@reduxjs/toolkit'

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

export const store = configureStore({
  reducer: {
    physicalStore: physicalStoreSlice.reducer,
  },
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch