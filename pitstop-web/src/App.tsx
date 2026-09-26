function App() {
  async function loadWashrooms() {
    const response=await fetch('http://localhost:5042/washrooms/nearby?latitude=43.65&longitude=-79.38')
    const data=await response.json()
    console.log(data)
  }


  return(
  <div>
    <h1>PitStop</h1>
    <p>Find a nearby washroom in Toronto</p>
    <button onClick={loadWashrooms}>Find Washrooms
    </button>
  </div>
)
  }
export default App