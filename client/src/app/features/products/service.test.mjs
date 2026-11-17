import assert from 'node:assert/strict'
import { afterEach, test } from 'node:test'
import { ApiProductService, ProductApiError } from './service.ts'

const originalFetch = globalThis.fetch

afterEach(() => {
  globalThis.fetch = originalFetch
})

test('list sends the API pagination and encoded search parameters', async () => {
  const signal = new AbortController().signal
  const result = { items: [], totalCount: 0, page: 2, pageSize: 10, totalPages: 0, hasNextPage: false, hasPreviousPage: true }
  globalThis.fetch = async (url, options) => {
    const requestUrl = new URL(url, 'http://localhost')
    assert.equal(requestUrl.pathname, '/api/Products')
    assert.equal(requestUrl.searchParams.get('page'), '2')
    assert.equal(requestUrl.searchParams.get('pageSize'), '10')
    assert.equal(requestUrl.searchParams.get('searchTerm'), 'boards & boots')
    assert.equal(options.signal, signal)
    return Response.json(result)
  }

  assert.deepEqual(await new ApiProductService().list(2, 'boards & boots', signal), result)
})

test('detail requests surface API errors instead of treating them as products', async () => {
  globalThis.fetch = async (url) => {
    assert.equal(url, '/api/Products/42')
    return new Response(null, { status: 404 })
  }

  await assert.rejects(
    new ApiProductService().get('42', new AbortController().signal),
    error => error instanceof ProductApiError && error.status === 404 && error.message === 'Product not found.',
  )
})
