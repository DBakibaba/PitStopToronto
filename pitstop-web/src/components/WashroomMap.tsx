import { MapContainer, TileLayer,Marker,Popup,useMap,CircleMarker} from "react-leaflet"
import type { Washroom } from "../types/Washroom"
import "leaflet/dist/leaflet.css"
import L from "leaflet"
import markerIcon from "leaflet/dist/images/marker-icon.png"
import markerIcon2x from "leaflet/dist/images/marker-icon-2x.png"
import markerShadow from "leaflet/dist/images/marker-shadow.png"
 
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

 

 

const washroomIcon = L.icon({
    iconUrl: markerIcon,
    iconRetinaUrl: markerIcon2x,
    shadowUrl: markerShadow,
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
})

 



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
       
    const MapCenter:[number,number]= latitude !==null && longitude !==null 
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


                <TileLayer url="https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png" attribution='&copy;
                <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; 
                <a href="https://carto.com/attributions">CARTO</a>'
/>
                {latitude !==null && longitude !== null &&(
                    <CircleMarker    
                        center={[latitude,longitude]} radius={10}>
                    </CircleMarker>
                )}

                {washrooms.map(washroom => {
                     
                    return (

                    <Marker 
                        key={washroom.id}
                        position={[washroom.latitude,washroom.longitude]}
                        icon={washroomIcon}
                >
                    <Popup>
                       <h3>{washroom.type === "Library"  ? `${washroom.name} Public Library`: washroom.name}</h3>
                         
                        <p>{washroom.address}</p>
                        <p>{washroom.distanceKm.toFixed(2)} km away </p>
                        <p>{washroom.hoursText}</p>
                         

                        <button disabled={latitude===null || longitude===null}
                        onClick={()=>{
                            if(latitude===null || longitude===null){
                                return
                            }
                        
                            openGoogleMaps(latitude,longitude,washroom.latitude,washroom.longitude)
                        }}
                    > Get Direction

                        </button>
                    </Popup>
                    
                   </Marker>
            )
    })}
            </MapContainer>
        
        )

}
export default WashroomMap 