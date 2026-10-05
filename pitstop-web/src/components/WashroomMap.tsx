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

    function openGoogleMaps(lat:number,long:number,wcLat:number,wcLong:number){
        const baseUrl="https://www.google.com/maps/dir/?api=1"
        const origin= `&origin=${lat},${long}`
        const destination = `&destination=${wcLat},${wcLong}`
        const travelMode = "&travelmode=driving"; 

        window.open(baseUrl + origin  + destination + travelMode, '_blank')

    }
                
                
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
                        <p>Hours:{washroom.hours}</p>
                        
                        {washroom.operatingHour.map((operatingHours)=>(
                        <p key={operatingHours.id}> {operatingHours.dayOfWeek} - {operatingHours.openTime} - {operatingHours.closeTime}
                        
                        </p>
                    ))}

                        <button onClick={()=>openGoogleMaps(latitude,longitude,washroom.latitude,washroom.longitude)}  
                            > Get Direction

                        </button>
                    </Popup>
                    
                    </Marker>
                ))}
            </MapContainer>
        
        )

}
export default WashroomMap 