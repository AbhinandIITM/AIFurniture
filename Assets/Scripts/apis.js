key = "Cedl5hujGrhswiPvzJj0u9pAkQNdOMLgC5GRiBH3yLhOJvfG5knt5Jo3igwg";

var myHeaders = new Headers();
myHeaders.append("Content-Type", "application/json");

var raw = JSON.stringify({
    "key": key,
    "image": "https://i.pinimg.com/736x/7c/83/64/7c83645c903677dd93ef50fe953dceea.jpg",
    "ss_sampling_steps" : 50,
    "slat_sampling_steps" : 50,
    "output_format":"glb",
    "webhook": null,
    "track_id": null,
    "temp": "no"
});

var requestOptions = {
  method: 'POST',
  headers: myHeaders,
  body: raw,
  redirect: 'follow'
};

fetch("https://modelslab.com/api/v6/3d/image_to_3d", requestOptions)
  .then(response => response.text())
  .then(result => console.log(result))
  .catch(error => console.log('error', error));