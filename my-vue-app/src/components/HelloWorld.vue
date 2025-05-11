// File: src/components/UserNearestHotel.vue
<template>
  <div class="p-4">
    <h1 class="text-xl font-bold mb-4">Users and Nearest Hotels</h1>

    <form @submit.prevent="addUser" class="mb-6">
      <h2 class="text-lg font-semibold mb-2">Add New User</h2>
      <input v-model="newUser.name" placeholder="Name" class="border p-1 mr-2" required />
      <input v-model="newUser.city" placeholder="City" class="border p-1 mr-2" required />
      <input v-model.number="newUser.lat" placeholder="Latitude" class="border p-1 mr-2" required />
      <input v-model.number="newUser.lon" placeholder="Longitude" class="border p-1 mr-2" required />
      <button type="submit" class="bg-blue-500 text-white px-2 py-1">Add User</button>
    </form>

    <ul>
      <li v-for="user in usersWithNearestHotel" :key="user.id" class="mb-2 border p-2 rounded">
        <p><strong>{{ user.name }}</strong> ({{ user.address.city }})</p>
        <p>Nearest Hotel: {{ user.nearestHotel.name }} ({{ user.nearestHotel.distance.toFixed(2) }} km)</p>
        <button @click="removeUser(user.id)" class="text-red-500 mt-1">Delete</button>
      </li>
    </ul>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from 'vue';
import axios from 'axios';

interface User {
  id: number;
  name: string;
  address: {
    city: string;
    geo: {
      lat: string;
      lng: string;
    }
  };
  nearestHotel?: { name: string; lat: number; lon: number; distance: number };
}

export default defineComponent({
  name: 'UserNearestHotel',
  setup() {
    const usersWithNearestHotel = ref<User[]>([]);
    const userIdCounter = ref(11); // Start after JSONPlaceholder IDs

    const newUser = ref({
      name: '',
      city: '',
      lat: 0,
      lon: 0
    });

    const hotels = [
      { name: 'A', lat: -43.9509, lon: -34.4618 },
      { name: 'B', lat: 40.7128, lon: -74.0060 },
      { name: 'C', lat: 34.0522, lon: -118.2437 },
      { name: 'D', lat: -25.2744, lon: 133.7751 }
    ];

    const haversineDistance = (lat1: number, lon1: number, lat2: number, lon2: number): number => {
      const toRad = (x: number) => (x * Math.PI) / 180;
      const R = 6371;
      const dLat = toRad(lat2 - lat1);
      const dLon = toRad(lon2 - lon1);
      const a =
        Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) *
        Math.sin(dLon / 2) * Math.sin(dLon / 2);
      const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
      return R * c;
    };

    const calculateNearestHotel = (lat: number, lon: number) => {
      return hotels
        .map(hotel => ({
          ...hotel,
          distance: haversineDistance(lat, lon, hotel.lat, hotel.lon)
        }))
        .sort((a, b) => a.distance - b.distance)[0];
    };

    const fetchUsers = async () => {
      const { data: users } = await axios.get('https://jsonplaceholder.typicode.com/users');
      const processed = users.map((user: User) => {
        const userLat = parseFloat(user.address.geo.lat);
        const userLon = parseFloat(user.address.geo.lng);

        return {
          ...user,
          nearestHotel: calculateNearestHotel(userLat, userLon)
        };
      });

      usersWithNearestHotel.value = processed;
    };

    const addUser = () => {
      const id = userIdCounter.value++;
      const user: User = {
        id,
        name: newUser.value.name,
        address: {
          city: newUser.value.city,
          geo: {
            lat: newUser.value.lat.toString(),
            lng: newUser.value.lon.toString()
          }
        },
        nearestHotel: calculateNearestHotel(newUser.value.lat, newUser.value.lon)
      };

      usersWithNearestHotel.value.push(user);
      newUser.value = { name: '', city: '', lat: 0, lon: 0 };
    };

    const removeUser = (id: number) => {
      usersWithNearestHotel.value = usersWithNearestHotel.value.filter(u => u.id !== id);
    };

    onMounted(fetchUsers);

    return { usersWithNearestHotel, newUser, addUser, removeUser };
  }
});
</script>

<style scoped>
ul {
  list-style-type: none;
  padding: 0;
}
</style>
