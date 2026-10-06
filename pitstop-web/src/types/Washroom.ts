export type Washroom={
  id:number
  name:string
  address:string | null
  distanceKm:number
  latitude:number
  longitude:number
  hours:string | null
  publicParking:string
  isAccessible:boolean
  operatingHours:OperatingHour[]
  
  
}

export type OperatingHour={
  id:number
  washroomId:number
  dayOfWeek:number  
  openTime:string | null
  closeTime:string  | null
  isClosed:boolean
}