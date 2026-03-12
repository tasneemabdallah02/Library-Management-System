//const calendarDays = document.getElementById("calendarDays");
//const monthYear = document.getElementById("monthYear");
//const modal = document.getElementById("bookingModal");

//let currentDate = new Date();

//function renderCalendar() {
//    calendarDays.innerHTML = "";

//    const year = currentDate.getFullYear();
//    const month = currentDate.getMonth();

//    const firstDay = new Date(year, month, 1);
//    const lastDay = new Date(year, month + 1, 0);

//    monthYear.innerText =
//        currentDate.toLocaleString("default", { month: "long" }) +
//        " " + year;

//    const startDay = firstDay.getDay();

//    for (let i = 0; i < startDay; i++)
//        calendarDays.innerHTML += `<div></div>`;

//    for (let day = 1; day <= lastDay.getDate(); day++) {

//        const fullDate = new Date(year, month, day);
//        const dateString = fullDate.toISOString().split("T")[0];

//        const dayDiv = document.createElement("div");
//        dayDiv.classList.add("day");

//        dayDiv.innerHTML = `<div>${day}</div>`;

//        const hasReservation = reservations.some(r =>
//            r.date === dateString
//        );

//        if (hasReservation)
//            dayDiv.innerHTML += `<div class="event-dot"></div>`;

//        dayDiv.addEventListener("click", () => {
//            document.getElementById("selectedDate").value = dateString;
//            modal.style.display = "flex";
//        });

//        calendarDays.appendChild(dayDiv);
//    }
//}

//function closeModal() {
//    modal.style.display = "none";
//}

//function saveBooking() {

//    const selectedDate = document.getElementById("selectedDate").value;
//    const startTime = document.getElementById("startTime").value;

//    if (!selectedDate || !startTime) {
//        alert("Please select date and start time.");
//        return;
//    }

//    const data = {
//        roomId: 1,
//        reservationDate: selectedDate,
//        startTime: startTime
//    };

//    fetch('/Rooms/Reserve', {
//        method: 'POST',
//        headers: {
//            'Content-Type': 'application/json'
//        },
//        body: JSON.stringify(data)
//    })
//        .then(res => {
//            if (!res.ok) throw new Error("Server error");
//            return res.json();
//        })
//        .then(result => {
//            if (result.success) {
//                alert("Reservation successful!");
//                location.reload();
//            } else {
//                alert(result.message);
//            }
//        })
//        .catch(err => {
//            alert("Something went wrong.");
//            console.error(err);
//        });
//}

//document.addEventListener("DOMContentLoaded", function () {
//    renderCalendar();
//});document.addEventListener("DOMContentLoaded", function () {
const calendarDays = document.getElementById("calendarDays");
const monthYear = document.getElementById("monthYear");
const modal = document.getElementById("bookingModal");

let currentDate = new Date();

function renderCalendar() {

    calendarDays.innerHTML = "";

    const year = currentDate.getFullYear();
    const month = currentDate.getMonth();

    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);

    monthYear.innerText =
        currentDate.toLocaleString("default", { month: "long" }) +
        " " + year;

    const startDay = firstDay.getDay();

    for (let i = 0; i < startDay; i++) {
        calendarDays.innerHTML += `<div></div>`;
    }

    for (let day = 1; day <= lastDay.getDate(); day++) {

        const fullDate = new Date(year, month, day);
        const dateString = fullDate.toISOString().split("T")[0];

        const dayDiv = document.createElement("div");
        dayDiv.classList.add("day");

        dayDiv.innerHTML = `<div>${day}</div>`;

        const hasReservation = reservations.some(r =>
            r.date === dateString
        );

        if (hasReservation) {
            dayDiv.innerHTML += `<div class="event-dot"></div>`;
        }

        dayDiv.addEventListener("click", () => {
            document.getElementById("selectedDate").value = dateString;
            modal.style.display = "flex";
        });

        calendarDays.appendChild(dayDiv);
    }
}

function closeModal() {
    modal.style.display = "none";
}

function saveBooking() {

    const selectedDate = document.getElementById("selectedDate").value;
    const startTime = document.getElementById("startTime").value;
    const endTime = document.getElementById("endTime").value;

    if (!selectedDate || !startTime || !endTime) {
        alert("Please fill all fields.");
        return;
    }

    const data = {
        roomId: 1,
        reservationDate: selectedDate,
        startTime: startTime,
        endTime: endTime
    };

    fetch('/Rooms/Reserve', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
        .then(res => res.json())
        .then(result => {
            if (result.success) {
                alert("Reservation successful!");
                location.reload();
            } else {
                alert(result.message);
            }
        })
        .catch(err => {
            alert("Something went wrong.");
            console.error(err);
        });
}

renderCalendar();




