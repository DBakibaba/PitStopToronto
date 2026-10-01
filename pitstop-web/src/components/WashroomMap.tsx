import { MapContainer, TileLayer,Marker,Popup,useMap,CircleMarker} from "react-leaflet"
import type { Washroom } from "../types/Washroom"
import "leaflet/dist/leaflet.css"
 
import { useEffect } from "react"


type WashroomMapProps={
    washrooms:Washroom[]
    latitude:number | null
    longitude:number | null

}
type MapControllerProps={
     latitude:number | null
     longitude:number | null
}

function MapController({latitude,longitude}:MapControllerProps){
    const map = useMap()
    useEffect(()=>{

        if(latitude !==null && longitude !==null){
                map.setView([latitude,longitude],13)
    }
    },[latitude,longitude,map])
    
    return null       
}
    
function WashroomMap({washrooms,latitude,longitude}:WashroomMapProps){
       
    const MapCenter= latitude !==null && longitude !==null 
                ? [latitude,longitude] :  [43.6532,-79.3832]
                
                
    return(
            <MapContainer 
         
                center={MapCenter}
                zoom={13}
                style={{ height:"500px", width:"100%"}}
               
                >
                <MapController  
                
                latitude={latitude}
                longitude={longitude}
                />


                <TileLayer  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                {latitude !==null && longitude !== null &&(
                    <CircleMarker    
                        center={[latitude,longitude]} radius={10}>
                    </CircleMarker>
                )}

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