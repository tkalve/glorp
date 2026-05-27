import { useState } from 'react'
import { GlorpClient, GlorpError } from './glorp/client'
import type { Bar, Foo } from './glorp/types'
import './App.css'

const client = new GlorpClient()

function App() {
  return (
    <main className="page">
      <header>
        <h1>Glorp</h1>
        <p>Send requests through the generated typed client.</p>
      </header>
      <FoosPanel />
      <BarsPanel />
    </main>
  )
}

function FoosPanel() {
  const [name, setName] = useState('blue')
  const [foos, setFoos] = useState<Foo[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  async function search(e: React.FormEvent) {
    e.preventDefault()
    setLoading(true)
    setError(null)
    try {
      const response = await client.send({ $type: 'FoosRequest', name })
      setFoos(response.data ?? [])
    } catch (err) {
      setError(formatError(err))
      setFoos(null)
    } finally {
      setLoading(false)
    }
  }

  return (
    <section className="panel">
      <h2>Foos</h2>
      <form onSubmit={search}>
        <label>
          Name contains
          <input value={name} onChange={(e) => setName(e.target.value)} />
        </label>
        <button type="submit" disabled={loading}>
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>
      {error && <p className="error">{error}</p>}
      {foos && (
        <ul className="grid">
          {foos.length === 0 && <li className="empty">No matches.</li>}
          {foos.map((foo) => (
            <li key={foo.name}>
              <span className="swatch" style={{ background: foo.color }} />
              <span className="label">{foo.name}</span>
              <code>{foo.color}</code>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}

function BarsPanel() {
  const [minHeight, setMinHeight] = useState(65)
  const [bars, setBars] = useState<Bar[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  async function search(e: React.FormEvent) {
    e.preventDefault()
    setLoading(true)
    setError(null)
    try {
      const response = await client.send({ $type: 'BarsRequest', minHeight })
      setBars(response.data ?? [])
    } catch (err) {
      setError(formatError(err))
      setBars(null)
    } finally {
      setLoading(false)
    }
  }

  return (
    <section className="panel">
      <h2>Bars</h2>
      <form onSubmit={search}>
        <label>
          Min height
          <input
            type="number"
            value={minHeight}
            onChange={(e) => setMinHeight(Number(e.target.value))}
          />
        </label>
        <button type="submit" disabled={loading}>
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>
      {error && <p className="error">{error}</p>}
      {bars && (
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Height</th>
              <th>Weight</th>
            </tr>
          </thead>
          <tbody>
            {bars.length === 0 && (
              <tr>
                <td colSpan={3} className="empty">
                  No matches.
                </td>
              </tr>
            )}
            {bars.map((bar) => (
              <tr key={bar.name}>
                <td>{bar.name}</td>
                <td>{bar.height}</td>
                <td>{bar.weight}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}

function formatError(err: unknown): string {
  if (err instanceof GlorpError) return `HTTP ${err.status}: ${err.body}`
  if (err instanceof Error) return err.message
  return String(err)
}

export default App
