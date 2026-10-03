import {useState} from 'react'
import WashroomMap from "./components/WashroomMap"
import type { Washroom } from "./types/Washroom"




function App() {
  const[washrooms,setWashrooms]=useState<Washroom[]>([])
  const[errorMessage,setErrorMessage]=useState<string>("")
  const[latitude,setLatitude]=useState<number | null>(null)
  const[longitude,setLongitude]=useState<number | null>(null)

  async function loadWashrooms() {
     
    navigator.geolocation.getCurrentPosition(
    async(position) => {
      const lat=position.coords.latitude
      const long=position.coords.longitude
      console.log("Browser location: ",lat,long)

      setLatitude(lat)
      setLongitude(long)
       
      const response=await fetch(`http://localhost:5042/washrooms/nearby?latitude=${lat}&longitude=${long}`)

      const data=await response.json()
      setWashrooms(data)},

      (error)=>{
      console.log(error)
      setErrorMessage("Location permission is required to find nearby washrooms.")
    
      
  })

  
}


  return(
  <div>
    <h1>PitStop</h1>
    
    
    <WashroomMap 
    washrooms={washrooms} 
    latitude={latitude}
    longitude={longitude}/> 
 
     
    <p>    </p>
    <p>Find a nearby washroom in Toronto</p>
    <button onClick={loadWashrooms}>Find Washrooms</button>
    <p>
      Washrooms found: {washrooms.length}</p> {errorMessage && <p>{errorMessage}</p>}
    {washrooms.map(washroom=>(
      <div key={washroom.id}>
        <h3>{washroom.name}</h3>
        <p>{washroom.address}</p>
        <p>{washroom.distanceKm.toFixed(2)} km away</p>
        <p>{washroom.hours}</p>
        <p>{washroom.publicParking}</p>
      </div>
    ))}
  </div>
)
  }
export default App