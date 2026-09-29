import {useState} from 'react'

type Washroom={
  id:number
  name:string
  address:string | null
  distanceKm:number
}

function App() {
  const[washrooms,setWashrooms]=useState<Washroom[]>([])
  const [errorMessage,setErrorMessage]=useState<string>("")

  async function loadWashrooms() {
     
    navigator.geolocation.getCurrentPosition(
    async(position) => {
      const latitude=position.coords.latitude
      const longitude=position.coords.longitude
       
      const response=await fetch(`http://localhost:5042/washrooms/nearby?latitude=${latitude}&longitude=${longitude}`)

      const data=await response.json()
      setWashrooms(data)},

      (error)=>{
      console.log(error)
      setErrorMessage("Location permission is required to find nearby washrooms.")
    
      
  })

  const [errorMessage,setErrorMessage]=useState<string>("")
}


  return(
  <div>
    <h1>PitStop</h1>
    <p>Find a nearby washroom in Toronto</p>
    <button onClick={loadWashrooms}>Find Washrooms
    </button>
    <p>
      Washrooms found: {washrooms.length}</p> {errorMessage && <p>{errorMessage}</p>}
    {washrooms.map(washroom=>(
      <div key={washroom.id}>
        <h3>{washroom.name}</h3>
        <p>{washroom.address}</p>
        <p>{washroom.distanceKm.toFixed(2)} km away</p>
      </div>
    ))}
  </div>
)
  }
export default App