import WashroomMap from "./components/WashroomMap";
import type { Washroom } from "./types/Washroom";
import "./App.css";
import torontoSkyline from "./assets/toronto-skyline.png";
import { useState, useEffect } from "react";
import exifr from "exifr";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

function App() {
  const [washrooms, setWashrooms] = useState<Washroom[]>([]);
  const [errorMessage, setErrorMessage] = useState<string>("");
  const [latitude, setLatitude] = useState<number | null>(null);
  const [longitude, setLongitude] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [hasSearched, setHasSearch] = useState<boolean>(false);
  const [activeView, setActiveView] = useState<"list" | "map">("list");
  const [selectedType, setSelectedType] = useState<number | null>(null);
  const [keyInput, setKeyInput] = useState("");
  const showAdminBox = new URLSearchParams(window.location.search).has("admin");
  const [adminKey, setAdminKey] = useState<string | null>(() =>
    localStorage.getItem("pitstopAdminKey"),
  );
  const [adminMessage, setAdminMessage] = useState<string>("");
  const [portableLatitude, setPortableLatitude] = useState<number | null>(null);
  const [portableLongitude, setPortableLongitude] = useState<number | null>(
    null,
  );
  const [needsLocationChoice, setNeedsLocationChoice] =
    useState<boolean>(false);

  function saveAdminKey() {
    const key = keyInput.trim();
    if (!key) {
      return;
    }
    localStorage.setItem("pitstopAdminKey", key);
    setAdminKey(key);
    setKeyInput("");
  }

  function logOutAdmin() {
    localStorage.removeItem("pitstopAdminKey");
    setAdminKey(null);
  }

  async function addPortableToilet() {
    if (portableLatitude === null || portableLongitude === null || !adminKey) {
      return;
    }

    try {
      const response = await fetch(`${apiBaseUrl}/washrooms/portable`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-Admin-Key": adminKey,
        },
        body: JSON.stringify({
          latitude: portableLatitude,
          longitude: portableLongitude,
        }),
      });

      if (response.status === 401) {
        setAdminMessage("Wrong admin key.");
        return;
      }
      if (!response.ok) {
        setAdminMessage(`Something went wrong (${response.status}).`);
        return;
      }
      setAdminMessage("Saved ✓");
    } catch (error) {
      setAdminMessage("Could not reach the server.");
    }
  }
  function handleLivePhoto(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];

    if (!file) {
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        setPortableLatitude(position.coords.latitude);
        setPortableLongitude(position.coords.longitude);
        setNeedsLocationChoice(false);
      },
      (error) => {
        console.error("Unable to get current location:", error.message);
      },
      {
        enableHighAccuracy: true,
      },
    );
  }

  async function readPhotoLocation(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];

    if (!file) {
      return;
    }

    try {
      const gps = await exifr.gps(file);

      if (gps) {
        setPortableLatitude(gps.latitude);
        setPortableLongitude(gps.longitude);
        setNeedsLocationChoice(false);

        console.log("Photo location:", gps.latitude, gps.longitude);
      } else {
        setPortableLatitude(null);
        setPortableLongitude(null);
        setNeedsLocationChoice(true);

        console.log("This photo does not contain GPS data.");
      }
    } catch (error) {
      console.error("Could not read photo metadata:", error);

      setPortableLatitude(null);
      setPortableLongitude(null);
      setNeedsLocationChoice(true);
    }
  }

  useEffect(() => {
    if (!hasSearched) {
      return;
    }
    const watchId = navigator.geolocation.watchPosition(
      (position) => {
        setLatitude(position.coords.latitude);
        setLongitude(position.coords.longitude);
      },

      (error) => {
        console.error("Location tracking failed:", error.message);
      },

      { enableHighAccuracy: true },
    );

    return () => {
      navigator.geolocation.clearWatch(watchId);
    };
  }, [hasSearched]);

  async function fetchWashrooms(
    lat: number,
    long: number,
    type: number | null,
  ) {
    setIsLoading(true);
    setErrorMessage("");

    let url = `${apiBaseUrl}/washrooms/nearby?latitude=${lat}&longitude=${long}`;

    if (type !== null) {
      url += `&type=${type}`;
    }

    try {
      const response = await fetch(url);

      if (!response.ok) {
        throw new Error(`Request failed: ${response.status}`);
      }

      const data = await response.json();
      setWashrooms(data);
      setHasSearch(true);
    } catch (error) {
      console.log(error);
      setErrorMessage("Unable to load washrooms. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }
  async function selectCategory(type: number | null) {
    if (latitude === null || longitude === null) {
      return;
    }

    setSelectedType(type);
    await fetchWashrooms(latitude, longitude, type);
  }
  async function loadWashrooms() {
    setIsLoading(true);
    setErrorMessage("");

    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const lat = position.coords.latitude;
        const long = position.coords.longitude;
        console.log("Browser location: ", lat, long);

        setLatitude(lat);
        setLongitude(long);

        await fetchWashrooms(lat, long, selectedType);
      },
      (error) => {
        console.log(error);
        setErrorMessage(
          "Location permission is required to find nearby washrooms.",
        );
        setIsLoading(false);
      },
    );
  }
  let searchButtonText = "Find Washrooms";

  if (isLoading) {
    searchButtonText = "Finding washrooms...";
  } else if (hasSearched) {
    searchButtonText = "Search again";
  }

  if (showAdminBox) {
    return (
      <div className="admin-panel">
        <h1>PitStop Admin</h1>

        {adminKey === null ? (
          <div>
            <label htmlFor="admin-key">Admin key</label>

            <input
              id="admin-key"
              type="password"
              value={keyInput}
              onChange={(event) => setKeyInput(event.target.value)}
            />

            <button onClick={saveAdminKey}>Enter admin mode</button>
          </div>
        ) : (
          <div className="admin-content">
            <p>Admin mode is active.</p>

            <div className="admin-photo-options">
              <section className="admin-option">
                <h2>At the location now</h2>

                <p>
                  Take a new photo and use your phone's current GPS location.
                </p>

                <label htmlFor="live-photo">Take photo now</label>

                <input
                  id="live-photo"
                  type="file"
                  accept="image/*"
                  capture="environment"
                  onChange={handleLivePhoto}
                />
              </section>

              <section className="admin-option">
                <h2>Upload an older photo</h2>

                <p>The photo must contain GPS information.</p>

                <label htmlFor="existing-photo">Choose existing photo</label>

                <input
                  id="existing-photo"
                  type="file"
                  accept="image/*"
                  onChange={readPhotoLocation}
                />
              </section>
            </div>

            {portableLatitude !== null && portableLongitude !== null && (
              <div className="location-result">
                <p>Location found.</p>
                <p>Latitude: {portableLatitude}</p>
                <p>Longitude: {portableLongitude}</p>
              </div>
            )}

            {needsLocationChoice && (
              <p className="location-error">
                This photo does not contain GPS data. Choose another photo or
                take a new photo at the location.
              </p>
            )}

            <div className="admin-actions">
              <button
                onClick={addPortableToilet}
                disabled={
                  portableLatitude === null || portableLongitude === null
                }
              >
                Add portable washroom
              </button>

              {adminMessage && <p>{adminMessage}</p>}

              <button onClick={logOutAdmin}>Log out</button>
            </div>
          </div>
        )}
      </div>
    );
  }
  return (
    <div>
      {showAdminBox && <p>Admin</p>}

      {hasSearched ? (
        <header className="results-header">
          <h1>PitStop</h1>

          <button
            className="search-again-button"
            onClick={loadWashrooms}
            disabled={isLoading}
          >
            {isLoading && (
              <span className="loading-spinner" aria-hidden="true" />
            )}
            {isLoading ? "Searching..." : "Search again"}
          </button>
        </header>
      ) : (
        <div>
          <h1 className="brand-name">PitStop</h1>

          <h2 className="hero-heading">
            A quick stop.
            <br />A better trip.
          </h2>

          <p className="hero-description">Find nearby washrooms in Toronto.</p>

          <img className="toronto-skyline" src={torontoSkyline} alt="" />

          <button
            className="find-button"
            onClick={loadWashrooms}
            disabled={isLoading}
          >
            {isLoading && (
              <span className="loading-spinner" aria-hidden="true" />
            )}
            {searchButtonText}
          </button>
        </div>
      )}

      {errorMessage && <p>{errorMessage}</p>}

      {hasSearched && !isLoading && washrooms.length === 0 && (
        <p>No washrooms found within 3 km.</p>
      )}

      {/* View buttons: visible when there are results */}
      {hasSearched && (
        <div className="category-filters">
          <button
            onClick={() => selectCategory(null)}
            aria-pressed={selectedType === null}
            disabled={isLoading}
          >
            All
          </button>

          <button
            onClick={() => selectCategory(8)}
            aria-pressed={selectedType === 8}
            disabled={isLoading}
          >
            Coffee
          </button>

          <button
            onClick={() => selectCategory(5)}
            aria-pressed={selectedType === 5}
            disabled={isLoading}
          >
            Grocery
          </button>

          <button
            onClick={() => selectCategory(3)}
            aria-pressed={selectedType === 3}
            disabled={isLoading}
          >
            Gas Stations
          </button>

          <button
            onClick={() => selectCategory(6)}
            aria-pressed={selectedType === 6}
            disabled={isLoading}
          >
            Retail
          </button>
        </div>
      )}

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

          {washrooms.map((washroom) => (
            <div key={washroom.id} className="washroom-card">
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
  );
}

export default App;
