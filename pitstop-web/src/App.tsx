import {useState} from 'react'

type Washroom={
  id:number
  name:string
  address:string | null
  distanceKm:number
}

function App() {
  const[washrooms,setWashrooms]=useState<Washroom[]>([])

  async function loadWashrooms() {
    const response=await fetch('http://localhost:5042/washrooms/nearby?latitude=43.65&longitude=-79.38')
    const data=await response.json()
    setWashrooms(data)
    console.log(data)

  }


  return(
  <div>
    <h1>PitStop</h1>
    <p>Find a nearby washroom in Toronto</p>
    <button onClick={loadWashrooms}>Find Washrooms
    </button>
    <p>Washrooms found:{washrooms.length}</p>
    {washrooms.map(washroom=>(
      <div key={washroom.id}>
        <h3>{washroom.name}</h3>
        <p>{washroom.address}</p>
        <p>{washroom.distanceKm}</p>
      </div>
    ))}
  </div>
)
  }
export default App