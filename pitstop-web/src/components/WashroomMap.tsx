import { MapContainer, TileLayer } from "react-leaflet"
import "leaflet/dist/leaflet.css"


    
function WashroomMap(){
        return(
            <MapContainer 
                center={[43.6532,-79.3832]} 
                zoom={13}
                style={{ height:"500px", width:"100x"}}
                >
                <TileLayer  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
            </MapContainer>
        
        )

}
export default WashroomMap 