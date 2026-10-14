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

 

const dayNames=[
    "Sunday",
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday"
]

function getTorontoClock() {
    const formatter = new Intl.DateTimeFormat("en-US", {
        timeZone: "America/Toronto",
        weekday: "long",
        hour: "2-digit",
        minute: "2-digit",
        hourCycle: "h23"
    })

    const parts = formatter.formatToParts(new Date())

    let weekday = ""
    let hour = 0
    let minute = 0

    for (const part of parts) {
        if (part.type === "weekday") {
            weekday = part.value
        } else if (part.type === "hour") {
            hour = Number(part.value)
        } else if (part.type === "minute") {
            minute = Number(part.value)
        }
    }

    return {
        day: dayNames.indexOf(weekday),
        currentMinutes: hour * 60 + minute
    }
}


function formatTime(time: string | null) : string {

    if(time === null){
        return ""
    }
    const[hour,minute] = time.split(":")
    const hourNumber=Number(hour)
    const period=hourNumber >=12? "PM" : "AM"
    let displayHour=hourNumber
    if (hourNumber >12){
        displayHour=hourNumber-12
    }
    if(hourNumber ===0){
        displayHour=12
    }

    return `${displayHour}:${minute} ${period}`
}

function isOpenNow(openTime:string | null ,closeTime:string | null, currentMinutes:number) : boolean {
    
    if(openTime===null || closeTime===null){
        return false
    }
     
    const [openHour, openMinute] = openTime.split(":")
    const openHourNum = Number(openHour)
    const openMinuteNum = Number(openMinute)

    const [closeHour, closeMinute] = closeTime.split(":")
    const closeHourNum=Number(closeHour)
    const closeMinuteNum=Number(closeMinute)

    const sumOpenMinuteNum=openHourNum*60+openMinuteNum
    const sumCloseMinuteNum=closeHourNum*60 + closeMinuteNum

     

   
    return currentMinutes >= sumOpenMinuteNum && currentMinutes < sumCloseMinuteNum

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
       
    const MapCenter:[number,number]= latitude !==null && longitude !==null 
                ? [latitude,longitude] :  [43.6532,-79.3832]

    function openGoogleMaps(lat:number,long:number,wcLat:number,wcLong:number){
        const baseUrl="https://www.google.com/maps/dir/?api=1"
        const origin= `&origin=${lat},${long}`
        const destination = `&destination=${wcLat},${wcLong}`
        const travelMode = "&travelmode=driving"; 

        window.open(baseUrl + origin  + destination + travelMode, '_blank')
        
    }
            
            const torontoClock = getTorontoClock()
            const today = torontoClock.day
               
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

                {washrooms.map(washroom => {

                            const todayOperatingHour = washroom.operatingHours.find(
                                operatingHour => operatingHour.dayOfWeek === today
                            )
                            let hoursText=washroom.hours?.trim() ? washroom.hours : "Hours unknown. Check before travelling."
                            console.log("START:", washroom.name, hoursText)

                            if(todayOperatingHour !=null){
                                 
                                if(todayOperatingHour.isClosed){
                                    hoursText="Closed today"
                                    
                                }else{
                                     const result=isOpenNow(todayOperatingHour.openTime,todayOperatingHour.closeTime,torontoClock.currentMinutes)
                                    
                                     if(result===true){
                                            hoursText=`Open until ${formatTime(todayOperatingHour.closeTime)}`
                                     }else{
                                        hoursText="Closed today"
                                     }
                                      
                                    
                                }
                                 
                            }
                               
                    return (

                    <Marker 
                        key={washroom.id}
                        position={[washroom.latitude,washroom.longitude]}
                >
                    <Popup>
                       <h3>{washroom.type === 2  ? `${washroom.name} Public Library`: washroom.name}</h3>
                         
                        <p>{washroom.address}</p>
                        <p>{washroom.distanceKm.toFixed(2)} km away </p>
                         
                      

                        <p> {hoursText}</p>

                        

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