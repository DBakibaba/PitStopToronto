import { MapContainer, TileLayer,Marker,Popup} from "react-leaflet"
import type { Washroom } from "../types/Washroom"
import "leaflet/dist/leaflet.css"
import { marker } from "leaflet"


type WashroomMapProps={
    washrooms:Washroom[]

}

    
function WashroomMap({washrooms}:WashroomMapProps){
        return(
            <MapContainer 
                center={[43.6532,-79.3832]} 
                zoom={13}
                style={{ height:"500px", width:"100x"}}
                >
                <TileLayer  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                {washrooms.map(washroom=>(
                    <Marker 
                        key={washroom.id}
                        position={[washroom.latitude,washroom.longitude]}
                >
                    <Popup>
                        <h3>{washroom.name}</h3>
                        <p>{washroom.address}</p>
                        <p>{washroom.distanceKm.toFixed(2)} km away </p>
                    </Popup>
                    
                    </Marker>
                ))}
            </MapContainer>
        
        )

}
export default WashroomMap 