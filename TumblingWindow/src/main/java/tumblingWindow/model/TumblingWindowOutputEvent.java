package tumblingWindow.model;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

/** TumblingWindowOutputEvent represents the events being generated. */
@Getter
@Setter
@AllArgsConstructor
@NoArgsConstructor
public class TumblingWindowOutputEvent {
    private String deviceId;
    private long startTime;
    private long endTime;
    private Double[] value;
}
