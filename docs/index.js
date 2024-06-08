// grab all our html elements
const buttons = document.getElementsByClassName("sidebutton");
const pages = document.getElementsByClassName("page");

// for every redirect button in our sidebar, add an event to open the page when clicked, and highlight the text
for (let button of buttons) button.addEventListener("click", function(e) {
  for (let b of buttons) {
    b.style.textDecoration = "";
    b.style.fontWeight = "";
  }
  button.style.textDecoration = "underline";
  button.style.fontWeight = "bold";
  
  for (let p of pages) p.style.display = "none";
  document.getElementById(button.id + "page").style.display = "block";
  
  document.getElementById("pageholder").scrollTop = 0;
});