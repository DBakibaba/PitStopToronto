import {useState} from 'react'
import WashroomMap from "./components/WashroomMap"
import type { Washroom } from "./types/Washroom"
import './App.css'
import torontoSkyline from "./assets/toronto-skyline.png"
import pitstopLogo from "./assets/pitstop-logo.png"

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

function App() {
  const[washrooms,setWashrooms]=useState<Washroom[]>([])
  const[errorMessage,setErrorMessage]=useState<string>("")
  const[latitude,setLatitude]=useState<number | null>(null)
  const[longitude,setLongitude]=useState<number | null>(null)
  const [isLoading, setIsLoading] = useState<boolean>(false)
  const[hasSearched,setHasSearch]=useState<boolean>(false)
  const [activeView, setActiveView] = useState<"list" | "map">("list")

  async function loadWashrooms() {
     
    setIsLoading(true)
    setErrorMessage("")
     

    navigator.geolocation.getCurrentPosition
    (
      
      async(position) => {
      const lat=position.coords.latitude
      const long=position.coords.longitude
      console.log("Browser location: ",lat,long)

      setLatitude(lat)
      setLongitude(long)
       
      try {
      const response = await fetch(
          `${apiBaseUrl}/washrooms/nearby?latitude=${lat}&longitude=${long}`
      )

      if (!response.ok) {
          throw new Error(`Request failed: ${response.status}`)
      }

      const data = await response.json()
      setWashrooms(data)
      setHasSearch(true)
      }
       catch (error) {
        console.log(error)
        setErrorMessage("Unable to load washrooms. Please try again.")
      }   
        finally {
        setIsLoading(false)
      
      }
    },
      (error)=>{
        console.log(error)
        setErrorMessage("Location permission is required to find nearby washrooms.")
        setIsLoading(false)
      }
    )
  }
  let searchButtonText = "Find Washrooms"

  if (isLoading) {
    searchButtonText = "Finding washrooms..."
  } else if (hasSearched) {
    searchButtonText = "Search again"
  }


   return (
    <div>
      <h1 className="brand-name">
          {hasSearched ? (
            <img
              className="pitstop-logo"
              src={pitstopLogo}
              alt="PitStop"
            />
          ) : (
            "PitStop"
          )}
      </h1>

      {/* Introduction: visible before the first successful search */}
      {!hasSearched && (
        <div>
          <h2 className="hero-heading">
            A quick stop.
            <br />
            A better trip.
          </h2>

          <p className="hero-description">
            Find nearby washrooms in Toronto.
          </p>

          <img
            className="toronto-skyline"
            src={torontoSkyline}
            alt=""
          />
        </div>
      )}

      {/* Search button: always visible */}
      <button
        className="find-button"
        onClick={loadWashrooms}
        disabled={isLoading}
      >
        {searchButtonText}
      </button>

      {errorMessage && <p>{errorMessage}</p>}

      {hasSearched && !isLoading && washrooms.length === 0 && (
        <p>No washrooms found within 3 km.</p>
      )}

      {/* View buttons: visible when there are results */}
      {hasSearched && washrooms.length > 0 && (
        <div className="view-switch">
          <button
            onClick={() => setActiveView("list")}
            aria-pressed={activeView === "list"}
          >
            List
          </button>

          <button
            onClick={() => setActiveView("map")}
            aria-pressed={activeView === "map"}
          >
            Map
          </button>
        </div>
      )}

      {/* Map view */}
      {hasSearched && activeView === "map" && (
        <WashroomMap
          washrooms={washrooms}
          latitude={latitude}
          longitude={longitude}
        />
      )}

      {/* List view */}
      {hasSearched && activeView === "list" && (
        <div>
          {washrooms.length > 0 && <p>Nearest washrooms:</p>}

          {washrooms.map(washroom => (
            <div key={washroom.id}className="washroom-card">
              <h3>
                {washroom.type === "Library"
                  ? `${washroom.name} Public Library`
                  : washroom.name}
              </h3>

              <p>{washroom.address}</p>
              <p>{washroom.distanceKm.toFixed(2)} km away</p>
              <p>{washroom.hoursText}</p>

              <a
                className="directions-button"
                href={`https://www.google.com/maps/dir/?api=1&destination=${washroom.latitude},${washroom.longitude}`}
                target="_blank"
                rel="noopener noreferrer"
              >
                Get Directions
              </a>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

  
export default App