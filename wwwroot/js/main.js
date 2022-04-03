window.onload = function () {
    const nums = [];
    const labels = [];
    const data = {
        labels: labels,
        datasets: [{
            label: 'цена окончания',
            backgroundColor: 'rgb(255,112,25)',
            
            data:nums,
            borderWidth: 2,
        }]
    };
    const config = {
        type: 'line',
        data: data,
        options: {}
    };
    const myChart = new Chart(
        document.getElementById('myChart'),
        config
    );
    let submitTicket = document.getElementById('sendData');
    submitTicket.onclick = test;
    function test() {
        let minDate = document.getElementById('minDate');
        let maxDate = document.getElementById('maxDate');
        let nameQ = document.getElementById('nameQ');
        let datas = {
            start: minDate.value,
            end: maxDate.value,
            name: nameQ.value
        }
        let xhr = new XMLHttpRequest();
        xhr.open('POST', '/getdata');
        xhr.setRequestHeader("Content-Type", "application/json;charset=UTF-8");

        let json = JSON.stringify(datas);
        xhr.send(json);
        alert(json);
        xhr.onload = function () {
            if (xhr.status == 200) {
                let obj = JSON.parse(xhr.response);
                let i = 1;
                obj.forEach((item) => {
                   
                    myChart.config.data.datasets[0].data.push(item.adjcloser);
                    myChart.config.data.labels.push(i)
                    i++;
                });
                myChart.update();
            }
        }
    }
    let getMoney =  document.getElementById('getMoney');
    getMoney.onclick =GetMoney;
    function GetMoney() {
        let minDate = document.getElementById('minDate');
        let maxDate = document.getElementById('maxDate');
        let nameQ = document.getElementById('nameQ');
        let answer = document.getElementById('answer');
        let datas = {
            start: minDate.value,
            end: maxDate.value,
            name: nameQ.value
        }
        let str1 = document.getElementById('selectss');
        alert(str1.value);
        let xhr = new XMLHttpRequest();
       
        xhr.open('Post', '/analyze' +str1.value);
        xhr.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
        let json = JSON.stringify(datas);
        xhr.send(json);
        xhr.onload = function () {
            if (xhr.status == 200) {
                if(xhr.response[0] ==="-"){
                    answer.style.color = "#d70000";
                    
                }else{
                    answer.style.color = "#4be700";
                }
                answer.innerHTML =xhr.response;
                myChart.update();
            }
        }
    }
}