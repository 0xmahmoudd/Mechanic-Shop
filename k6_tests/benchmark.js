import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '5s', target: 20 }, // Ramp up to 20 users
    { duration: '15s', target: 20 }, // Stay at 20 users for 15s
    { duration: '5s', target: 0 },  // Ramp down to 0 users
  ],
};

export default function () {
  const res = http.get('http://localhost:5190/api/work-orders?pageNumber=1&pageSize=20');
  
  check(res, {
    'status is 200': (r) => r.status === 200,
  });
  
  sleep(1);
}
