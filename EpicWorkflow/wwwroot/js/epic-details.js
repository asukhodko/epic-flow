$(document).ready(function () {

    var chartDoneness;
    var chartContributors;
    var chartContributorsTime;

        var chartColors = {
            red: 'rgb(245, 79, 132)',
            orange: 'rgb(245, 139, 64)',
            yellow: 'rgb(245, 205, 26)',
            green: 'rgb(65, 172, 192)',
            greeny: 'rgb(65, 222, 192)',
            blue: 'rgb(44, 142, 235)',
            blueblue: 'rgb(0, 42, 235)',
            purple: 'rgb(143, 82, 255)',
            grey: 'rgb(191, 183, 207)',
            redl: 'rgb(255, 159, 172)',
            yellowl: 'rgb(255, 245, 56)',
            green1: 'rgb(85, 215, 255)',
            greeny1: 'rgb(85, 255, 255)',
            bluel: 'rgb(64, 222, 255)',
            blueblue1: 'rgb(2, 82, 255)',
            purple1: 'rgb(163, 162, 255)',
            grey1: 'rgb(211, 255, 247)'
        };
    var chartBackgroundColors = [
        chartColors.red,
        chartColors.orange,
        chartColors.yellow,
        chartColors.green,
        chartColors.greeny,
        chartColors.blue,
        chartColors.blueblue,
        chartColors.purple,
        chartColors.grey,
        chartColors.redl,
        chartColors.yellowl,
        chartColors.green1,
        chartColors.greeny1,
        chartColors.blueblue1,
        chartColors.purple1,
        chartColors.grey1,
        chartColors.red,
        chartColors.orange,
        chartColors.yellow,
        chartColors.green,
        chartColors.greeny,
        chartColors.blue,
        chartColors.blueblue,
        chartColors.purple,
        chartColors.grey,
        chartColors.redl,
        chartColors.yellowl,
        chartColors.green1,
        chartColors.greeny1,
        chartColors.blueblue1,
        chartColors.purple1,
        chartColors.grey1
    ];
    var donenessChartColors = {
        expected: 'rgb(118, 168, 0)',
        doneness: 'rgb(37, 183, 255)',
        deadline: 'rgb(255, 64, 64)',
        today: 'rgb(192, 192, 192)'
    };

        var epicId = $('#epicId').val();

        refresh();

        function refresh() {
            initCharts();
        }

    function initCharts() {
            $.ajax(
                {
                    "url": '/api/v1/EpicDetails/chart/' + epicId,
                    "type": "GET",
                    "dataSrc": function (responseSerialized) {
                        return responseSerialized;
                    },
                    "datatype": "json",
                    "error": function (result) {
                        $.renderModalError("Произошла ошибка: " + result.status + ", " + result.statusText);
                    },
                    success: function (data) {
                        console.log('chart', data);
                        renderCharts(data);
                    }
                }
            );
        }

    function renderCharts(data) {
        renderDonenessChart(data);
        renderContributorsChart(data.contributors);
        renderContributorsTimeChart(data.contributorsWithTime);
    }

    function renderDonenessChart(data) {
            var doneness = _.map(data.doneness, function (item) {
                return {x: moment(item.x), y: item.y, updater: item.updater, increment: item.increment};
            });

            var expected = _.map(data.expected, function (item) {
                return {x: moment(item.x), y: item.y};
            });

            var deadline = _.map(data.deadline, function (item) {
                return {x: moment(item.x), y: item.y};
            });

        var today = _.map(data.today, function (item) {
            return {x: moment(item.x), y: item.y};
        });

            var datasets = [];
            if (deadline.length > 0) {
                datasets.push({
                    label: "Deadline",
                    data: deadline,
                    fill: false,
                    borderColor: donenessChartColors.deadline
                });
            }
            datasets.push({
                label: "% готово",
                data: doneness,
                fill: true,
                cubicInterpolationMode: 'monotone',
                borderColor: donenessChartColors.doneness,
                pointRadius: 2
            });
        datasets.push({
            label: "Сегодня",
            data: today,
            fill: true,
            borderColor: donenessChartColors.today
        });
            if (expected.length > 0) {
                datasets.push({
                    label: "Прогноз",
                    data: expected,
                    fill: false,
                    borderColor: donenessChartColors.expected
                });
            }

            var config = {
                type: 'line',
                data: {
                    datasets: datasets
                },
                options: {
                    responsive: true,
                    tooltips: {
                        callbacks: {
                            afterBody: function (t, data) {
                                var di = t[0].datasetIndex;
                                var i = t[0].index;
                                var row = data.datasets[di].data[i];
                                if (row.updater) {
                                    var increment = row.increment.toFixed(1);
                                    if (increment >= 0) {
                                        increment = '+' + increment;
                                    }
                                    return row.updater + ' (' + increment + '%)';
                                } else {
                                    return undefined;
                                }
                            }
                        }
                    },
                    title: {
                        display: true,
                        text: 'Прогресс'
                    },
                    scales: {
                        xAxes: [{
                            type: 'time',
                            time: {
                                // unit: 'day'
                            },
                            scaleLabel: {
                                display: true,
                                labelString: 'Время'
                            }
                        }],
                        yAxes: [{
                            scaleLabel: {
                                display: true,
                                labelString: '% готово'
                            },
                            ticks: {
                                max: 100,
                                min: 0,
                                stepSize: 5
                            }
                        }]
                    }
                }
            };

        chartDoneness = new Chart(document.getElementById("epicDonenessChart").getContext('2d'), config);

    }

    function renderContributorsChart(contributors) {
        var config = {
            type: 'polarArea',
            data: {
                labels: _.map(contributors, function (item) {
                    return item.contributor;
                }),
                datasets: [{
                    data: _.map(contributors, function (item) {
                        return item.impactValue.toFixed(1);
                    }),
                    backgroundColor: chartBackgroundColors
                }]
            },
            options: {
                responsive: true,
                legend: {
                    position: 'right',
                },
                title: {
                    display: true,
                    text: 'Суммарный вклад участников'
                }
            }
        };

        chartContributors = new Chart(document.getElementById("contributorsChart").getContext('2d'), config);
        }

    function renderContributorsTimeChart(contributorsWithTime) {
        console.log('contributorsChart', contributorsWithTime);

        var color = Chart.helpers.color;
        var colorIdx = -1;
        var datasets = _.map(contributorsWithTime, function (item) {
            if (colorIdx++ >= chartColors.length)
                colorIdx = 0;
            return {
                label: item.contributor,
                data: _.map(item.contributions, function (contribution) {
                    return {
                        x: moment(contribution.date),
                        y: contribution.impactValue
                    };
                }),
                fill: true,
                cubicInterpolationMode: 'monotone',
                backgroundColor: color(chartBackgroundColors[colorIdx]).alpha(0.3).rgbString(),
                borderColor: chartBackgroundColors[colorIdx],
                borderWidth: 1,
                pointRadius: 1,
                pointHoverRadius: 3
            };
        });

        console.log("datasets", datasets);

        var config = {
            type: 'line',
            data: {
                datasets: datasets
            },
            options: {
                responsive: true,
                legend: {
                    position: 'right'
                },
                title: {
                    display: true,
                    text: 'Вклад участников во времени'
                },
                scales: {
                    xAxes: [{
                        type: 'time',
                        time: {
                            unit: 'day'
                        },
                        scaleLabel: {
                            display: true,
                            labelString: 'Время'
                        }
                    }],
                    yAxes: [{
                        scaleLabel: {
                            display: true,
                            labelString: 'Points'
                        },
                        ticks: {
                            min: 0,
                            stepSize: 5
                        }
                    }]
                }
            }
        };

        chartContributorsTime = new Chart(document.getElementById("contributorsTimeChart").getContext('2d'), config);
    }

    }
);
