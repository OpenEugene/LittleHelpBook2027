[Home](../Home.md) / [Specifications](../Specifications.md) / Providers <!-- wikidown:breadcrumb -->

# Providers

A provider is an organization or service listed in the Little Help Book, such as a food pantry, clinic or shelter.

## What a provider record holds

- A name and a description of what the provider offers.
- How to reach them: website, email, hours, and any number of phone numbers.
- Any number of addresses, each noting wheelchair access and language support.
- Whether the listing is active. Inactive providers stay on record but are not meant to be shown to the public.

## Address suggestions and map coordinates

When an editor adds or changes an address, the street field can suggest matching US addresses through Mapbox. Choosing a suggestion fills the city, state and postal code and stores its longitude, latitude and Mapbox identifier with the address. Editing the street afterward clears those coordinates so an outdated point is not shown on the map.

The address form still accepts manual entry when Mapbox is not configured or a suitable suggestion is unavailable.

## Provider map

When a Mapbox public access token is configured, the providers page first asks **Where are you now?** A person can enter an address, city or ZIP code for Mapbox to find, or explicitly choose **Use my device location**. The browser asks for location permission only after that button is selected. A manual or device choice replaces any location estimated from the person's public IP. Mapbox receives manual searches or device coordinates to identify the selected location. If permission is denied or the device location is unavailable or times out, the person can continue with manual entry.

After three or more characters are typed, Mapbox offers up to five matching US addresses, streets, ZIP codes, cities, localities or neighborhoods. Selecting a suggestion immediately sets and stores the location and updates the map and distances.

With a keyboard, **Arrow Down** moves from the location field into the suggestions, the arrow keys move through them, **Enter** selects the highlighted suggestion, and **Escape** dismisses the list. If suggestions are unavailable or none is selected, the typed text can still be submitted with **Set location** or **Enter**.

The selected location—its coordinates, label, source and selection time—is not stored by the site or server. It is kept only in that browser's local storage for up to 30 days and restored when the page is refreshed. **Clear** removes the saved location from the browser.

If there is no valid saved location, the page automatically asks IPWho for an approximate city, region, country and coordinates based on the person's public IP, then saves that approximate result in the browser. If the lookup fails, manual entry and the device-location option remain available. An IP-derived location is only an estimate. The interface always discloses that the location is stored locally, that IPWho processes the public IP for this estimate, and that Mapbox processes location searches and approximate area labels.

The page then asks **What do you need?** The person chooses a category and, optionally, a subcategory. Their selected location stays in place while these need filters change, and the map updates to show the active, geocoded addresses of matching active providers.

Pins use meaningful symbols and colors based on the most specific matching subcategory, with the category used as a fallback. Selecting a pin identifies the provider and address, lists all relevant services, shows the approximate distance from the selected location, and links to the provider's details. A provider with more than one geocoded active address can have more than one pin.

A summary reports how many locations are mapped, how many providers those locations represent, and how many providers in the current results do not have a geocoded active address. The map is hidden when no Mapbox public token is configured.

Alongside the map, the nearby-provider list appears in a responsive MudBlazor drawer anchored to the right. It remains persistently beside the map on desktop and opens over the map on smaller screens. Accessible toggle and close controls let people show or hide it, and Mapbox automatically resizes as the drawer's width changes so the map remains fully usable.

The list shows each mapped provider once. If a provider has several mapped addresses, it uses the closest one. Before a location is chosen, providers are sorted alphabetically; afterward, they are sorted by approximate straight-line distance.

Each entry includes the same meaningful service symbol and color used on the map, the provider name with a link to its details, the selected address, relevant services, and the approximate distance when available. An accessible **Show on map** control focuses the matching pin and opens its popup.

The list updates with the category and subcategory choices and provides a useful message when no mapped providers match the current needs.

The location and need questions appear over a welcoming community banner, giving people a clear and encouraging entry point for finding help. The map and nearby-provider results remain on a clean surface below so locations and service information are easy to read.

## Categories

A provider can be in several categories and several subcategories. Choosing a subcategory always places the provider in that subcategory's category as well, so a provider never appears under a subcategory without its category.

## Who can do what

- Anyone who can view the page can browse providers and filter them by category.
- People with edit permission can add, change and delete providers.
- Deleting a provider also removes its addresses, phone numbers and category choices.
